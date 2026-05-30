<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useNotify } from '@/shared/composables/useNotify'
import { ROUTE_NAMES } from '@/router'
import { apiRequest } from '@/shared/utils/ApiRequest'
import CollapsibleSection from '@/shared/components/CollapsibleSection.vue'

const route = useRoute()
const router = useRouter()
const notify = useNotify()

const username = ref<string>('')
const isLoading = ref<boolean>(false)

const login = async function () {
  if (username.value.trim() == '') {
    notify.failure('Username cannot be empty.')
    return
  }

  try {
    isLoading.value = true

    const result = await apiRequest<{ token: string }>('/api/account/login', {
      method: 'POST',
      body: JSON.stringify({ username: username.value }),
    })

    if (!result.token) {
      throw new Error('Session token not received.')
    }

    localStorage.setItem('token', result.token.toString())

    router.push({ name: ROUTE_NAMES.VOCABULARY })
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  if (route.query.sessionExpired === 'true') {
    notify.info('Session expired. Please login again')
  }
})
</script>

<template>
  <CollapsibleSection title="Login">
    <form @submit.prevent="login">
      <input class="input" type="text" placeholder="Username..." v-model="username" />
      <button class="big-button" type="submit" :disabled="isLoading">Login</button>
    </form>
  </CollapsibleSection>
</template>

<style lang="scss" scoped>
button {
  margin-top: 20px;
}
</style>
