<script setup lang="ts">
import { ref } from 'vue'
import RadioItem from '@/features/launcher/components/LauncherRadioItem.vue'
import { apiRequest } from '@/shared/utils/ApiRequest'
import router, { ROUTE_NAMES } from '@/router'

const selectedMode = ref<string>('fast')

async function onSubmit() {
  await apiRequest<string>(`/api/session?mode=${selectedMode.value}`, {
    method: 'POST',
  })

  router.push({ name: ROUTE_NAMES.SESSION })
}
</script>

<template>
  <form @submit.prevent="onSubmit">
    <div class="mode-grid">
      <RadioItem
        v-model="selectedMode"
        value="fast"
        title="Fast session"
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
