<script setup lang="ts">
import type { RepetitionDay } from '../types/RepetitionDay'

const props = defineProps<{ data: RepetitionDay }>()

const date = new Date(props.data.date)
const isPlanned = props.data.vocabularyForeigns.length !== 0
</script>

<template>
  <div class="day" :class="{ 'day--planned': isPlanned }">
    <span>{{ date.getDate() }}</span>
    <span>{{ date.toLocaleString('en-EN', { month: 'short' }).toLowerCase() + '.' }}</span>
  </div>
</template>

<style lang="scss" scoped>
.day {
  cursor: default;

  display: flex;
  flex-direction: column;
  justify-content: center;
  text-align: center;
  box-shadow: 5px 5px 0px $shadow;

  color: $gray-font;
  background-color: $plane-white;

  height: 65px;
  max-width: 60px;
  border-radius: 12px;

  font-size: 16px;

  &--planned {
    @include lift();

    color: $black-font;
  }
}
</style>
