import { defineStore } from 'pinia'
import { ref } from 'vue'
import type {
  CreateEntryRequest,
  VocabularyEntry,
  PatchEntryRequest,
} from '../types/VocabularyEntry'
import { apiRequest } from '@/shared/utils/ApiRequest'
import { useLoadingPlaceholder } from '@/shared/composables/useLoadingPlaceholder'
import type { PageValue } from '../types/PageValue'

export const useVocabularyEntryStore = defineStore('entry', () => {
  const entries = ref<VocabularyEntry[]>([])
  const totalPages = ref<number>(1)

  const loadingPlaceholder = useLoadingPlaceholder()

  async function fetchPage(
    guid: string | null,
    startLetter: string,
    endLetter: string,
    page: number,
    pageSize: number = 10,
  ) {
    try {
      if (entries.value.length == 0) loadingPlaceholder.startSkeleton()
      else loadingPlaceholder.startLoading(page > 1)

      const result = await apiRequest<PageValue<VocabularyEntry>>(
        `/api/vocabularies/${guid}/entries/${startLetter}-${endLetter}?page=${page}&pageSize=${pageSize}`,
      )

      if (page === 1) entries.value = result.items
      else entries.value = entries.value.concat(result.items)

      totalPages.value = result.totalPages
    } finally {
      loadingPlaceholder.stopLoading()
    }
  }

  async function resetPages() {
    entries.value = []
    totalPages.value = 1
  }

  async function searchEntries(guid: string | null, query: string): Promise<VocabularyEntry[]> {
    try {
      loadingPlaceholder.startLoading()
      return await apiRequest<VocabularyEntry[]>(`/api/vocabularies/${guid}/entries?query=${query}`)
    } finally {
      loadingPlaceholder.stopLoading()
    }
  }

  async function createEntry(guid: string, body: CreateEntryRequest) {
    const result = await apiRequest<VocabularyEntry>(`/api/vocabularies/${guid}/entries/`, {
      method: 'POST',
      body: JSON.stringify(body),
    })

    entries.value.push(result)
  }

  async function patchEntry(guid: string | null, id: number, body: PatchEntryRequest) {
    const result = await apiRequest<VocabularyEntry>(`/api/vocabularies/${guid}/entries/${id}`, {
      method: 'PATCH',
      body: JSON.stringify(body),
    })

    const index = entries.value.findIndex((e) => e.id === result.id)
    if (index !== -1) {
      entries.value.splice(index, 1, result)
    }
  }

  async function deleteEntry(guid: string | null, deleteId: number) {
    // const translationsCount = entries.value.find((e) => e.id === deleteId)?.translations.length || 0
    entries.value = entries.value.filter((e) => e.id !== deleteId)

    await apiRequest<boolean>(`/api/vocabularies/${guid}/entries/${deleteId}`, {
      method: 'DELETE',
    })
  }

  return {
    entries,
    totalPages,
    loadingPlaceholder,
    fetchPage,
    resetPages,
    searchEntries,
    createEntry,
    patchEntry,
    deleteEntry,
  }
})
