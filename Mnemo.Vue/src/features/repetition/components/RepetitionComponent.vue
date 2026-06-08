<script lang="ts" setup>
import { computed, onMounted } from 'vue'
import RepetitionItem from './RepetitionItem/RepetitionItem.vue'
import { useRepetitionStore } from '../stores/RepetitionStore.ts'
import router, { ROUTE_NAMES } from '@/router/index.ts'

const repetition = useRepetitionStore()

const tasks = computed(() => repetition.tasks)

let lastAction: number = Date.now()

function onSubmitAnswer(id: number, answer: string) {
  const timeNow = Date.now()
  const elapsedTimeMilliseconds = timeNow - lastAction

  repetition.submitAnswer(id, answer, elapsedTimeMilliseconds)
  lastAction = timeNow
}

function onFinish() {
  if (repetition.isFinished) {
    router.push({ name: ROUTE_NAMES.VOCABULARY })
  } else {
    repetition.finishTasks()
  }
}

onMounted(async () => {
  if (repetition.tasks.length == 0) await repetition.fetchTasks()
})
</script>

<template>
  <RepetitionItem
    v-for="(task, index) in tasks"
    :key="task.id"
    :list-number="index + 1"
    :task="task"
    :disabled="repetition.isFinished"
    @submit-answer="onSubmitAnswer"
  />
  <button class="big-button" type="button" @click="onFinish">
    {{ repetition.isFinished ? 'Back' : 'Finish' }}
  </button>
</template>

<style lang="scss" scoped></style>
