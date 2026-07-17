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

  notify.custom(`planned ${props.data.vocabularyForeigns.length} entries`, formatted)
}
</script>

<template>
  <div class="day" :class="{ 'day--planned': isPlanned }" @click="showPlannedForeigns()">
    <div v-show="data.isImportantDay" class="important-mark">*</div>
    <span>{{ date.getDate() }}</span>
    <span>{{ date.toLocaleString('en-EN', { month: 'short' }).toLowerCase() + '.' }}</span>
  </div>
</template>

<style lang="scss" scoped>
.day {
  position: relative;

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

  .important-mark {
    position: absolute;

    top: 6px;
    right: 12px;

    color: $black-font;

    font-size: 18px;
    font-weight: 200;
  }

  &--planned {
    @include lift();

    cursor: pointer;

    color: $black-font;
  }
}
</style>
