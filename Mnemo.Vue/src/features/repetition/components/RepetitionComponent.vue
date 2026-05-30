<script lang="ts" setup>
import { onMounted, ref } from 'vue'
import RepetitionItem from './RepetitionItem/RepetitionItem.vue'
import type { RepetitionTask } from '../types/RepetitionTask.ts'
import { apiRequest } from '@/shared/utils/ApiRequest.ts'

const isLoading = ref<boolean>(false)
const tasks = ref<RepetitionTask[]>([])

async function fetchRepetition() {
  try {
    isLoading.value = true
    tasks.value = await apiRequest<RepetitionTask[]>('/api/session/tasks/')
  } finally {
    isLoading.value = false
  }
}

function onSubmitAnswer(id: number, answer: string) {
  apiRequest<RepetitionTask>(`/api/session/tasks/${id}`, {
    method: 'POST',
    body: JSON.stringify({ answer: answer }),
  })
}

onMounted(async () => {
  await fetchRepetition()
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
