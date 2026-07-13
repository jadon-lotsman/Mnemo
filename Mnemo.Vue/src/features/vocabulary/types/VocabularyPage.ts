import type { VocabularyEntry } from './VocabularyEntry'

export interface VocabularyPage {
  entries: VocabularyEntry[]
  page: number
  pageSize: number
  totalPages: number
  totalEntries: number
  totalTranslations: number
}
