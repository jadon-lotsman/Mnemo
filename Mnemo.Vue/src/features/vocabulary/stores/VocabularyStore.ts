import { defineStore } from 'pinia'
import type { VocabularyHeader } from '../types/VocabularyHeader'
import { ref } from 'vue'
import type { PageValue } from '../types/PageValue'
import { apiRequest } from '@/shared/utils/ApiRequest'
import { useLoadingPlaceholder } from '@/shared/composables/useLoadingPlaceholder'
import type { VocabularyRange } from '../types/VocabularySector'

export const useVocabularyStore = defineStore('vocabularies', () => {
  const headers = ref<VocabularyHeader[]>([])
  const ranges = ref<VocabularyRange[]>([])

  const loadingPlaceholder = useLoadingPlaceholder()

  async function fetchHeadersPage(page: number, pageSize: number = 10) {
    try {
      const isFirstPage: boolean = page === 1
      loadingPlaceholder.startLoading(!isFirstPage)

      if (page * pageSize < headers.value.length) return

      const result = await apiRequest<PageValue<VocabularyHeader>>(
        `/api/vocabularies?page=${page}&pageSize=${pageSize}`,
      )

      headers.value?.push(...result.items)
    } finally {
      loadingPlaceholder.stopLoading()
    }
  }

  async function fetchRanges(guid: string | null, isDescending: boolean) {
    try {
      loadingPlaceholder.startLoading()

      const result = await apiRequest<VocabularyRange[]>(
        `/api/vocabularies/${guid}/sectors?isDescending=${isDescending}`,
      )

      ranges.value = result
    } finally {
      loadingPlaceholder.stopLoading()
    }
  }

  return {
    headers,
    ranges,
    loadingPlaceholder,
    fetchHeadersPage,
    fetchRanges,
  }
})
