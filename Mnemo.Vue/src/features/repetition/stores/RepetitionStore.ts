import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { apiRequest } from '@/shared/utils/ApiRequest'
import type { RepetitionTask } from '../types/RepetitionTask'
import type { RepetitionResult } from '../types/RepetitionResult'

export const useRepetitionStore = defineStore('repetition', () => {
  const tasks = ref<RepetitionTask[]>([])
  const isLoading = ref<boolean>(false)
  const isFinished = ref<boolean>(false)

  const totalTasks = computed(() => tasks.value.length)

  async function fetchTasks() {
    try {
      isLoading.value = true
      tasks.value = await apiRequest<RepetitionTask[]>('/api/repetition/tasks/')
    } finally {
      isFinished.value = false
      isLoading.value = false
    }
  }

  async function finishRepetition() {
    try {
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
    } finally {
      isFinished.value = true
    }
  }

  return {
    tasks,
    totalTasks,
    isLoading,
    isFinished,
    fetchTasks,
    finishRepetition,
  }
})
