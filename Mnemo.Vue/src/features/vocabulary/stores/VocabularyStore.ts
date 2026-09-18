import { defineStore } from 'pinia'
import type { VocabularyHeader } from '../types/VocabularyHeader'
import { ref } from 'vue'
import type { PageValue } from '../types/PageValue'
import { apiRequest } from '@/shared/utils/ApiRequest'
import { useLoadingPlaceholder } from '@/shared/composables/useLoadingPlaceholder'
import type { VocabularyRange } from '../types/VocabularySector'

export const useVocabularyStore = defineStore('vocabularies', () => {
  const loadingPlaceholder = useLoadingPlaceholder()

  const headers = ref<VocabularyHeader[]>([])

  async function fetchHeadersPage(page: number, pageSize: number = 10) {
    try {
      const isFirstPage: boolean = page === 1
      loadingPlaceholder.startLoading(!isFirstPage)

      if (page * pageSize < headers.value.length) return

      const result = await apiRequest<PageValue<VocabularyHeader>>(
        `/api/vocabularies?page=${page}&pageSize=${pageSize}`,
      )

      console.log(result)

      headers.value?.push(...result.items)
    } finally {
      loadingPlaceholder.stopLoading()
    }
  }

  async function fetchRanges(
    guid: string,
    isDescending: boolean = false,
  ): Promise<VocabularyRange[]> {
    try {
      loadingPlaceholder.startLoading()

      const result = await apiRequest<VocabularyRange[]>(
        `/api/vocabularies/${guid}/sectors?isDescending=${isDescending}`,
      )

      return result
    } finally {
      loadingPlaceholder.stopLoading()
    }
  }

  return {
    headers,
    fetchHeadersPage,
    fetchRanges,
  }
})
