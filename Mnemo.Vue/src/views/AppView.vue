<script setup lang="ts">
import Counter from '../components/ClickCounter.vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const logout = () => {
  localStorage.removeItem('token')
  router.push('/login')
}
</script>

<template>
  <main>
    <Counter />
    <button @click="logout">Logout</button>
  </main>
</template>

<style scoped></style>
