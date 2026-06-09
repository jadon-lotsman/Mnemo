<script setup lang="ts">
import { computed, onMounted } from 'vue'
import CalendarItem from './CalendarItem.vue'
import { useCalendarStore } from '../stores/CalendarStore.ts'

const calendar = useCalendarStore()

const days = computed(() => calendar.days)

onMounted(async () => {
  if (calendar.days.length === 0) await calendar.fetchDays()
})
</script>

<template>
  <div class="calendar">
    <CalendarItem v-for="day in days" :key="day.date" :data="day" />
  </div>
</template>

<style lang="scss" scoped>
.calendar {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(60px, 1fr));
  gap: 7px;
}
</style>
