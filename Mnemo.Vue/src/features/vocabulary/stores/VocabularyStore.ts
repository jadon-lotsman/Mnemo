import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { VocabularyEntry } from '../types/VocabularyEntry'
import { apiRequest } from '@/shared/utils/ApiRequest'

export const useVocabularyStore = defineStore('vocabulary', () => {
  const entries = ref<VocabularyEntry[]>([])
  const isLoading = ref<boolean>(false)

  const totalEntries = computed(() => entries.value.length)
  const totalTranslations = computed(() =>
    entries.value.reduce((sum, entry) => sum + entry.translations.length, 0),
  )

  async function fetchVocabulary() {
    try {
      isLoading.value = true

      entries.value = await apiRequest<VocabularyEntry[]>('/api/vocabulary/entries/')
    } finally {
      isLoading.value = false
    }
  }

  async function searchVocabulary(foreign: string): Promise<VocabularyEntry[]> {
    try {
      isLoading.value = true

      const responseData = await apiRequest<VocabularyEntry>(
        `/api/Vocabulary/entries/find?foreign=${foreign}`,
      )

      return [responseData]
    } catch {
      return []
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
    searchVocabulary,
  }
})
