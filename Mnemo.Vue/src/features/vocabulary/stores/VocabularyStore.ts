import { defineStore } from 'pinia'
import { ref } from 'vue'
import type {
  CreateEntryRequest,
  VocabularyEntry,
  PatchEntryRequest,
} from '../types/VocabularyEntry'
import { apiRequest } from '@/shared/utils/ApiRequest'
import type { VocabularyPage, VocabularySector } from '../types/VocabularySection'
import { useLoadingPlaceholer } from '@/shared/composables/useLoadingPlaceholder'

export const useVocabularyStore = defineStore('vocabulary', () => {
  const entries = ref<VocabularyEntry[]>([])
  const sectors = ref<VocabularySector[]>([])

  const totalPages = ref<number>()
  const totalEntries = ref<number>()
  const totalTranslations = ref<number>()

  const loadingPlaceholder = useLoadingPlaceholer()

  async function fetchSectors() {
    const result = await apiRequest<VocabularySector[]>(`/api/vocabulary/entries/sectors`)

    sectors.value = result
  }

  async function fetchPage(startWord: string, endWord: string) {
    try {
      loadingPlaceholder.startLoading()
      const result = await apiRequest<VocabularyPage>(
        `/api/vocabulary/entries?startWord=${startWord}&endWord=${endWord}`,
      )

      entries.value = result.entries
      totalEntries.value = result.totalEntries
      totalTranslations.value = result.totalTranslations
    } finally {
      loadingPlaceholder.stopLoading()
    }
  }

  async function searchVocabulary(query: string): Promise<VocabularyEntry[]> {
    try {
      loadingPlaceholder.startLoading()
      return await apiRequest<VocabularyEntry[]>(`/api/Vocabulary/entries/search?query=${query}`)
    } finally {
      loadingPlaceholder.stopLoading()
    }
  }

  async function addEntry(body: CreateEntryRequest) {
    const result = await apiRequest<VocabularyEntry>('/api/vocabulary/entries/', {
      method: 'POST',
      body: JSON.stringify(body),
    })

    entries.value.push(result)
  }

  async function patchEntry(id: number, body: PatchEntryRequest) {
    const result = await apiRequest<VocabularyEntry>(`/api/vocabulary/entries/${id}`, {
      method: 'PATCH',
      body: JSON.stringify(body),
    })

    const index = entries.value.findIndex((e) => e.id === result.id)
    if (index !== -1) {
      entries.value.splice(index, 1, result)
    }
  }

  async function deleteEntry(deleteId: number) {
    entries.value = entries.value.filter((e) => e.id !== deleteId)

    await apiRequest<VocabularyEntry>(`/api/vocabulary/entries/${deleteId}`, {
      method: 'DELETE',
    })
  }

  return {
    entries,
    sectors,
    totalPages,
    totalEntries,
    totalTranslations,
    loadingPlaceholder,
    addEntry,
    patchEntry,
    deleteEntry,
    fetchSectors,
    fetchPage,
    searchVocabulary,
  }
})
