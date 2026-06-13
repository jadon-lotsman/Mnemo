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
  partOfSpeech?: string
  foreign: string
  transcription?: string
  examples?: string[]
  translations: string[]
}

export interface VocabularyPatchRequest {
  partOfSpeech?: string
  foreign?: string
  transcription?: string
  examplesAdd?: string[]
  examplesRemove?: string[]
  translationsAdd?: string[]
  translationsRemove?: string[]
  synonymsAdd?: string[]
  synonymsRemove?: string[]
  antonymsAdd?: string[]
  antonymsRemove?: string[]
}
