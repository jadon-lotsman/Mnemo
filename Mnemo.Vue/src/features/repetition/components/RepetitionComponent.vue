<script lang="ts" setup>
import { computed, onMounted } from 'vue'
import RepetitionItem from './RepetitionItem/RepetitionItem.vue'
import type { RepetitionTask } from '../types/RepetitionTask.ts'
import { apiRequest } from '@/shared/utils/ApiRequest.ts'
import { useRepetitionStore } from '../stores/RepetitionStore.ts'

const repetition = useRepetitionStore()

const tasks = computed(() => repetition.tasks)
let lastAction: number = Date.now()

function onSubmitAnswer(id: number, answer: string) {
  const timeNow = Date.now()
  const elapsedTimeMilliseconds = timeNow - lastAction

  apiRequest<RepetitionTask>(`/api/repetition/tasks/${id}`, {
    method: 'POST',
    body: JSON.stringify({
      UserAnswer: answer,
      ElapsedTimeMilliseconds: elapsedTimeMilliseconds,
    }),
  })

  lastAction = timeNow
}

onMounted(async () => {
  if (repetition.tasks.length == 0) await repetition.fetchTasks()
})
</script>

<template>
  <RepetitionItem
    v-for="(task, index) in tasks"
    :key="task.id"
    :list-number="index"
    :task="task"
    @submit-answer="onSubmitAnswer"
  />
</template>

<style lang="scss" scoped></style>
