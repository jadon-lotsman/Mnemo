<script setup lang="ts">
import { useNotify } from '@/shared/composables/useNotify'
import type { RepetitionDay } from '../types/RepetitionDay'

const notify = useNotify()

const props = defineProps<{ data: RepetitionDay }>()

const date = new Date(props.data.date)
const isPlanned = props.data.vocabularyForeigns.length !== 0

function showPlannedForeigns() {
  if (!isPlanned) return

  const formatted = props.data.vocabularyForeigns.map((word) => `'${word}'`).join(', ')

  notify.custom(`planned at date`, formatted)
}
</script>

<template>
  <div class="day" :class="{ 'day--planned': isPlanned }" @click="showPlannedForeigns()">
    <span>{{ date.getDate() }}</span>
    <span>{{ date.toLocaleString('en-EN', { month: 'short' }).toLowerCase() + '.' }}</span>
  </div>
</template>

<style lang="scss" scoped>
.day {
  cursor: default;
  user-select: none;

  display: flex;
  flex-direction: column;
  justify-content: center;
  text-align: center;
  box-shadow: 5px 5px 0px $shadow;

  color: $shadow;
  background-color: $plane-white;

  height: 65px;
  max-width: 60px;
  border-radius: 12px;

  font-size: 16px;

  &--planned {
    @include lift();

    cursor: pointer;

    color: $black-font;
  }
}
</style>
