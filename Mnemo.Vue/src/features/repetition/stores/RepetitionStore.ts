import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { apiRequest } from '@/shared/utils/ApiRequest'
import type { RepetitionTask } from '../types/RepetitionTask'
import type { RepetitionResult } from '../types/RepetitionResult'

export const useRepetitionStore = defineStore('repetition', () => {
  const tasks = ref<RepetitionTask[]>([])

  const isFinished = computed(
    () => tasks.value.length == 0 || tasks.value[tasks.value.length - 1]?.quality != null,
  )
  const totalTasks = computed(() => tasks.value.length)

  async function isExists(): Promise<boolean> {
    return (await apiRequest<{ inProcess: string }>('/api/repetition/')).inProcess == 'true'
  }

  async function createTasks(mode: string): Promise<boolean> {
    tasks.value = await apiRequest<RepetitionTask[]>(`/api/repetition?mode=${mode}`, {
      method: 'POST',
    })

    return true
  }

  async function finishTasks() {
    const result = await apiRequest<RepetitionResult>('/api/repetition/', {
      method: 'DELETE',
    })

    for (const taskResult of result.taskResults) {
      const task = tasks.value.find((t) => t.id === taskResult.id)
      if (task) {
        task.isCorrect = taskResult.isCorrect
        task.quality = taskResult.quality
        task.correctAnswer = taskResult.correctAnswer
      }
    }
  }

  async function fetchTasks() {
    tasks.value = await apiRequest<RepetitionTask[]>('/api/repetition/tasks/')
  }

  async function submitAnswer(id: number, answer: string, elapsedTimeMilliseconds: number) {
    apiRequest<RepetitionTask>(`/api/repetition/tasks/${id}`, {
      method: 'POST',
      body: JSON.stringify({
        UserAnswer: answer,
        ElapsedTimeMilliseconds: elapsedTimeMilliseconds,
      }),
    })
  }

  return {
    tasks,
    totalTasks,
    isFinished,
    isExists,
    createTasks,
    fetchTasks,
    submitAnswer,
    finishTasks,
  }
})
