<script lang="ts" setup>
import router, { ROUTE_NAMES } from '@/router'
import { apiRequest } from '@/shared/utils/ApiRequest'

function deleteSession() {
  apiRequest<string>('/api/session', {
    method: 'DELETE',
  })

  router.push({ name: ROUTE_NAMES.VOCABULARY })
}
</script>

<template>
  <button class="big-button" type="button" @click="deleteSession">Finish</button>
</template>

<style lang="scss" scoped></style>
