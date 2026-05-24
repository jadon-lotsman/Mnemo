import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { VocabularyEntry } from '../types/VocabularyEntry'
import { useNotify } from '@/features/notify/hooks/useNotify'

export const useVocabularyStore = defineStore('vocabulary', () => {
  const notify = useNotify()

  const entries = ref<VocabularyEntry[]>([])
  const isLoading = ref<boolean>(false)

  const totalEntries = computed(() => entries.value.length)
  const totalTranslations = computed(() =>
    entries.value.reduce((sum, entry) => sum + entry.translations.length, 0),
  )

  const fetchVocabulary = async function () {
    isLoading.value = true

    try {
      const response = await fetch('/api/vocabulary/entries/', {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('token')}`,
        },
      })

      if (!response.ok) {
        let errorText = 'Vocabulary error'
        try {
          const errorData = await response.json()
          errorText = errorData.title || errorText
        } catch {
          errorText = `${response.status}: ${response.statusText}`
        }
        throw new Error(errorText)
      }

      const responseData: VocabularyEntry[] = await response.json()

      entries.value = responseData
    } catch (err: unknown) {
      const error = err as Error
      notify.failure(error.message ?? 'Unknown error...')
    } finally {
      isLoading.value = false
    }
  }

  return {
    entries,
    totalEntries,
    totalTranslations,
    isLoading,
    fetchVocabulary,
  }
})
