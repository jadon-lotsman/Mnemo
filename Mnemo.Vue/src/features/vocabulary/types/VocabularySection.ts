import type { VocabularyEntry } from './VocabularyEntry'

export interface VocabularySector {
  label: string
  startWord: string
  endWord: string
  count: number
}

export interface VocabularyPage {
  entries: VocabularyEntry[]
  totalEntries: number
  totalTranslations: number
}
