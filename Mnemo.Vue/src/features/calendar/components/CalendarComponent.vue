<script setup lang="ts">
import { onMounted, ref } from 'vue'
import CalendarItem from './CalendarItem.vue'
import type { RepetitionDay } from '../types/RepetitionDay.ts'
import { apiRequest } from '@/shared/utils/ApiRequest.ts'
import { fillMissingDays } from '../utils/FillMissingDays.ts'

const isLoading = ref<boolean>(false)
const calendar = ref<RepetitionDay[]>([])

const fetchSchedule = async function () {
  isLoading.value = true

  try {
    const responseData = await apiRequest<RepetitionDay[]>('/api/repetition/states')
    calendar.value = fillMissingDays(responseData)
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  fetchSchedule()
})
</script>

<template>
  <div class="calendar">
    <CalendarItem v-for="day in calendar" :key="day.date" :data="day" />
  </div>
</template>

<style lang="scss" scoped>
.calendar {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(60px, 1fr));
  gap: 7px;
}
</style>
