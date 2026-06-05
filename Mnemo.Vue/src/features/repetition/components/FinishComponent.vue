<script lang="ts" setup>
import router, { ROUTE_NAMES } from '@/router'
import { useRepetitionStore } from '../stores/RepetitionStore'
import { computed } from 'vue'

const repetition = useRepetitionStore()

const buttonText = computed(() => (repetition.isFinished ? 'Goodbye!' : 'Finish'))

async function onSubmit() {
  if (repetition.isFinished) {
    router.push({ name: ROUTE_NAMES.VOCABULARY })
  } else {
    repetition.finishRepetition()
  }
}
</script>

<template>
  <button class="big-button" type="button" @click="onSubmit">{{ buttonText }}</button>
</template>

<style lang="scss" scoped></style>
