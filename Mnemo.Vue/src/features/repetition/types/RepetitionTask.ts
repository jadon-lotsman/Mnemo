interface BaseTask {
  id: number
  prompt: string
  taskType: string
  userAnswer: string
}

interface TextTask extends BaseTask {
  taskType: 'text'
}

interface OptionTask extends BaseTask {
  taskType: 'option'
  options: string[]
}

interface OrderPartsTask extends BaseTask {
  taskType: 'parts'
  sentenceParts: string[]
}

interface YesOrNoTask extends BaseTask {
  taskType: 'yesorno'
  option: string
}

export type RepetitionTask = TextTask | OptionTask | OrderPartsTask | YesOrNoTask
