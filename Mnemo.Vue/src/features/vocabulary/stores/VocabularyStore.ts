import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type {
  CreateEntryRequest,
  VocabularyEntry,
  PatchEntryRequest,
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

  async function searchVocabulary(query: string): Promise<VocabularyEntry[]> {
    try {
      isLoading.value = true
      return await apiRequest<VocabularyEntry[]>(`/api/Vocabulary/entries/search?query=${query}`)
    } finally {
      isLoading.value = false
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
