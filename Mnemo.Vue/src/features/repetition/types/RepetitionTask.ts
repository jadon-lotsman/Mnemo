interface BaseTask {
  id: number
  partOfSpeech?: string
  prompt: string
  taskType: string
  userAnswer: string
  quality?: number
  isCorrect?: boolean
  correctAnswer?: string
}

interface TextTask extends BaseTask {
  taskType: 'text'
}

interface OptionTask extends BaseTask {
  taskType: 'option'
  options: string[]
}

interface SentenceReorderTask extends BaseTask {
  taskType: 'sentence'
  sentenceParts: string[]
}

interface SyllableReorderTask extends BaseTask {
  taskType: 'syllable'
  syllables: string[]
}

interface YesOrNoTask extends BaseTask {
  taskType: 'yesorno'
  option: string
}

export type RepetitionTask =
  | TextTask
  | OptionTask
  | SentenceReorderTask
  | SyllableReorderTask
  | YesOrNoTask
