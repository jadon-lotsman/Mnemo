import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { RepetitionDay } from '../types/RepetitionDay'
import { apiRequest } from '@/shared/utils/ApiRequest'
import { fillMissingDays } from '../utils/FillMissingDays'

export const useCalendarStore = defineStore('calendar', () => {
  const days = ref<RepetitionDay[]>([])

  async function fetchDays() {
    const result = await apiRequest<RepetitionDay[]>('/api/repetition/states')
    days.value = fillMissingDays(result)
  }

  return {
    days,
    fetchDays,
  }
})
