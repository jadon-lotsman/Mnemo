<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useNotify } from '@/features/notify/hooks/useNotify'

const router = useRouter()
const notify = useNotify()

const username = ref<string>('')
const isLoading = ref<boolean>(false)

const login = async function () {
  if (username.value.trim() == '') {
    notify.failure('Username cannot be empty.')
    return
  }

  isLoading.value = true

  try {
    const response = await fetch('/api/account/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        username: username.value,
      }),
    })

    if (!response.ok) {
      let errorText = 'Login error'
      try {
        const errorData = await response.json()
        errorText = errorData.title || errorText
      } catch {
        errorText = `${response.status}: ${response.statusText}`
      }
      throw new Error(errorText)
    }

    const data = await response
    const token = data.text()

    if (!token) {
      throw new Error('Session token not received.')
    }

    localStorage.setItem('token', (await token).toString())

    router.push('/app')
  } catch (err: unknown) {
    const error = err as Error
    notify.failure(error.message ?? 'Unknown error...')
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <form @submit.prevent="login">
    <input type="text" v-model="username" />
    <button type="submit" :disabled="isLoading">Login</button>
  </form>
</template>

<style lang="scss" scoped></style>
