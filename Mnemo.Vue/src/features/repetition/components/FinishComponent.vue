<script lang="ts" setup>
import router, { ROUTE_NAMES } from '@/router'
import { apiRequest } from '@/shared/utils/ApiRequest'

function deleteRepetition() {
  apiRequest<string>('/api/repetition/', {
    method: 'DELETE',
  })

  router.push({ name: ROUTE_NAMES.VOCABULARY })
}
</script>

<template>
  <button class="big-button" type="button" @click="deleteRepetition">Finish</button>
</template>

<style lang="scss" scoped></style>
