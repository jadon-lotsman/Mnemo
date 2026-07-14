import type { VocabularyEntry } from './VocabularyEntry'

export interface VocabularySector {
  label: string
  startWord: string
  endWord: string
  count: number
}

export interface VocabularyPage {
  entries: VocabularyEntry[]
  hasMore: boolean
  sectorEntries: number
}

export interface VocabularyStatistics {
  totalEntries: number
  totalTranslations: number
}
