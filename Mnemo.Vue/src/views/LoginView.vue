<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const username = ref<string>('')
const isLoading = ref<boolean>(false)
const errorMessage = ref<string>('')

const login = async function () {
  if (username.value.trim() == '') {
    errorMessage.value = 'Поле не должно быть пустым'
    return
  }

  isLoading.value = true

  try {
    const response = await fetch('/api/Account/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        username: username.value,
      }),
    })

    if (!response.ok) {
      // Обрабатываем ошибки (401, 422, 500 и т.д.)
      let errorText = 'Ошибка входа'
      try {
        const errorData = await response.json()
        errorText = errorData.title || errorText
      } catch {
        errorText = `Ошибка ${response.status}: ${response.statusText}`
      }
      throw new Error(errorText)
    }

    const data = await response
    const token = data.text()

    if (!token) {
      throw new Error('Сервер не вернул токен')
    }

    // Сохраняем токен в localStorage
    localStorage.setItem('token', (await token).toString())

    // Перенаправляем пользователя на главную страницу приложения
    router.push('/app')
  } catch (err: unknown) {
    const error = err as Error
    errorMessage.value = error.message ?? 'Не удалось войти. Попробуйте позже.'
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <form @submit.prevent="login">
    <input type="text" v-model="username" />
    <button type="submit" :disabled="isLoading">Login</button>
    <h1 v-text="errorMessage"></h1>
  </form>
</template>

<style scoped></style>
