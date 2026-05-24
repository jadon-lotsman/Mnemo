<script setup lang="ts">
import { onMounted, ref } from 'vue'
import CalendarDay from './CalendarDay.vue'
import type { RepetitionDay } from '../types/RepetitionDay'
import { useNotify } from '@/features/notify/hooks/useNotify'

const notify = useNotify()

const isLoading = ref<boolean>(false)
const calendarData = ref<RepetitionDay[]>([])

const fetchSchedule = async function () {
  isLoading.value = true

  try {
    const response = await fetch('/api/schedule/', {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`,
      },
    })

    if (!response.ok) {
      let errorText = 'Calendar error'
      try {
        const errorData = await response.json()
        errorText = errorData.title || errorText
      } catch {
        errorText = `${response.status}: ${response.statusText}`
      }
      throw new Error(errorText)
    }

    const responseData: RepetitionDay[] = await response.json()

    calendarData.value = fillMissingDays(responseData)
  } catch (err: unknown) {
    const error = err as Error
    notify.failure(error.message ?? 'Unknown error...')
  } finally {
    isLoading.value = false
  }
}

function fillMissingDays(days: RepetitionDay[]): RepetitionDay[] {
  if (days.length === 0) return []

  const firstDateStr = days[0]?.date
  const lastDateStr = days[days.length - 1]?.date

  const startDate = new Date(firstDateStr ?? '')
  const endDate = new Date(lastDateStr ?? '')

  let diff = Math.round((endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24))
  if (diff < 6) {
    diff = 6
  }

  const totalDays = diff

  const daysMap = new Map<string, RepetitionDay>()
  for (const day of days) {
    daysMap.set(day.date, day)
  }

  const result: RepetitionDay[] = []
  const current = new Date(startDate)

  for (let i = 0; i < totalDays; i++) {
    const dateStr = formatDate(current)
    const existing = daysMap.get(dateStr)

    if (existing) {
      result.push(existing)
    } else {
      result.push({
        date: dateStr,
        vocabularyForeigns: [],
      })
    }

    current.setDate(current.getDate() + 1)
  }

  return result
}

function formatDate(date: Date): string {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

onMounted(() => {
  fetchSchedule()
})
</script>

<template>
  <div class="calendar">
    <CalendarDay v-for="dayInfo in calendarData" :key="dayInfo.date" :data="dayInfo"></CalendarDay>
  </div>
</template>

<style lang="scss" scoped>
.calendar {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(60px, 1fr));
  gap: 7px;
}
</style>
