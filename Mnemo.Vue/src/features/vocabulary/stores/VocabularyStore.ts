import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type {
  VocabularyCreateRequest,
  VocabularyEntry,
  VocabularyPatchRequest,
} from '../types/VocabularyEntry'
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

  async function addEntry(body: VocabularyCreateRequest): Promise<VocabularyEntry> {
    const result = await apiRequest<VocabularyEntry>('/api/vocabulary/entries/', {
      method: 'POST',
      body: JSON.stringify(body),
    })

    return result
  }

  async function patchEntry(id: number, body: VocabularyPatchRequest): Promise<VocabularyEntry> {
    const result = await apiRequest<VocabularyEntry>(`/api/vocabulary/entries/${id}`, {
      method: 'PATCH',
      body: JSON.stringify(body),
    })

    return result
  }

  async function deleteEntry(id: number) {
    await apiRequest<VocabularyEntry>(`/api/vocabulary/entries/${id}`, {
      method: 'DELETE',
    })
  }

  return {
    entries,
    totalEntries,
    totalTranslations,
    isLoading,
    addEntry,
    patchEntry,
    deleteEntry,
    fetchVocabulary,
    searchVocabulary,
  }
})
