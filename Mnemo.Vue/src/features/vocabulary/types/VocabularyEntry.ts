export interface VocabularyEntry {
  id: number
  partOfSpeech: string
  foreign: string
  transcription: string
  transcriptionAudioUrl: string
  examples: string[]
  translations: string[]
  synonyms: string[]
  antonyms: string[]
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
