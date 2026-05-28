export interface VocabularyEntry {
  id: number
  foreign: string
  transcription: string
  examples: string[]
  translations: string[]
}

export interface VocabularyCreateRequest {
  foreign: string
  transcription?: string
  examples?: string[]
  translations: string[]
}

export interface VocabularyPatchRequest {
  foreign?: string
  transcription?: string
  examplesAdd?: string[]
  examplesRemove?: string[]
  translationsAdd?: string[]
  translationsRemove?: string[]
}
