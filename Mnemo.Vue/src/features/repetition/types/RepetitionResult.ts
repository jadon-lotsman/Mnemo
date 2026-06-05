export interface RepetitionResult {
  correct: number
  total: number
  percent: number
  totalTimeMilliseconds: number
  taskResults: TaskResult[]
}

export interface TaskResult {
  id: number
  quality: number
  isCorrect: boolean
  correctAnswer: string
}
