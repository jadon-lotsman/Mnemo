<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useNotify } from '@/shared/composables/useNotify'
import { apiRequest } from '@/shared/utils/ApiRequest'
import CollapsibleSection from '@/shared/components/CollapsibleSection.vue'
import { ROUTE_NAMES } from '@/shared/constants/RouteConst'
import { useLoadingPlaceholder } from '@/shared/composables/useLoadingPlaceholder'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const loadingPlaceholder = useLoadingPlaceholder()

const username = ref<string>('')
const buttonText = computed(() => (loadingPlaceholder.showSkeleton.value ? 'Logining...' : 'Login'))

async function login(username: string) {
  if (username.trim() == '') {
    notify.failure('Username cannot be empty.')
    return
  }

  try {
    loadingPlaceholder.startLoading()

    const result = await apiRequest<{ token: string }>('/api/account/login', {
      method: 'POST',
      body: JSON.stringify({ username: username }),
    })

    if (!result.token) {
      throw new Error('Session token not received.')
    }

    localStorage.setItem('token', result.token.toString())

    router.push({ name: ROUTE_NAMES.VOCABULARY })
  } finally {
    loadingPlaceholder.stopLoading()
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
    <form @submit.prevent="login(username)">
      <input
        class="input"
        autocapitalize="none"
        type="text"
        placeholder="Username..."
        v-model="username"
      />
      <button class="big-button" type="submit" :disabled="loadingPlaceholder.isLoading.value">
        {{ buttonText }}
      </button>
    </form>

    <div class="guest-container">
      <span>Don't have a username?</span>
      <span class="guest-text" @click="login('jadon')">Login as Guest</span>
    </div>
  </CollapsibleSection>
</template>

<style lang="scss" scoped>
button {
  margin-top: 15px;
}

.input {
  transition:
    transform 0.2s,
    box-shadow 0.2s ease;
  box-shadow: 5px 5px 0px $shadow-color;

  border: $surface-secondary 3px solid;

  border-radius: 12px;
  padding: 10px;

  width: 100%;
  color: $text-primary;

  font-size: 15px;

  &:focus {
    transform: translateY(-3px);
    box-shadow: 8px 8px 0px $shadow-color;
  }
}

.guest-container {
  display: flex;
  flex-direction: column;
  align-items: center;

  gap: 6px;

  margin-top: 30px;

  color: $text-muted;

  .guest-text {
    cursor: pointer;

    color: $text-secondary;

    font-size: 16px;

    text-decoration: underline;
  }
}
</style>
