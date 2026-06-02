<script setup lang="ts">
import { ref } from 'vue'
import RadioItem from '@/features/launcher/components/LauncherRadioItem.vue'
import { apiRequest } from '@/shared/utils/ApiRequest'
import router, { ROUTE_NAMES } from '@/router'
import type { RepetitionTask } from '@/features/repetition/types/RepetitionTask'
import { useNotify } from '@/shared/composables/useNotify'

const notify = useNotify()

const selectedMode = ref<string>('fast')

async function onSubmit() {
  const result = await apiRequest<{ inProcess: string }>('/api/repetition/')

  if (result.inProcess) {
    notify.success('Repetition already exists. Redirecting...')

    router.push({ name: ROUTE_NAMES.REPETITION })
  } else {
    await apiRequest<RepetitionTask[]>(`/api/repetition?mode=${selectedMode.value}`, {
      method: 'POST',
    })

    router.push({ name: ROUTE_NAMES.REPETITION })
  }
}
</script>

<template>
  <form @submit.prevent="onSubmit">
    <div class="mode-grid">
      <RadioItem
        v-model="selectedMode"
        value="fast"
        title="Fast repetition"
        description="Random words without save results"
      />
      <RadioItem
        v-model="selectedMode"
        value="planned"
        title="Planned at today"
        description="Planned words with save results"
      />
    </div>

    <button type="submit" class="big-button">Start</button>
  </form>
</template>

<style lang="scss" scoped>
.mode-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(40%, 1fr));

  gap: 10px;
  margin-bottom: 15px;
}
</style>
