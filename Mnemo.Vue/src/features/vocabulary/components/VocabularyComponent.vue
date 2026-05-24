<script setup lang="ts">
import { ref, onMounted } from 'vue'

const responseData = ref(null)
const isLoading = ref<boolean>(false)

const fetchEntries = async function () {
  isLoading.value = true

  try {
    const response = await fetch('/api/vocabulary/entries/', {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`,
      },
    })

    responseData.value = await response.json()
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  fetchEntries()
})
</script>

<template>
  <div v-if="isLoading">Loading...</div>
  <div v-else-if="responseData == null">No content</div>
  <div v-else>{{ JSON.stringify(responseData, null, 2) }}</div>
</template>

<style lang="scss" scoped></style>
