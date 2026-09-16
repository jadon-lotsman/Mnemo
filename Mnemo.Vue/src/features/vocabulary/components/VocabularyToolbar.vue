<script setup lang="ts">
import { ref, watch } from 'vue'

defineProps<{
  isLoading: boolean
}>()

const emit = defineEmits<{
  (e: 'submitSearch', query: string): void
  (e: 'clickCreate'): void
}>()

const searchQuery = ref<string>('')
const inputRef = ref<HTMLInputElement | null>(null)

function createForm() {
  emit('clickCreate')
}

function submitSearch() {
  emit('submitSearch', searchQuery.value)
}

watch(
  () => searchQuery.value,
  (newVal, oldVal) => {
    if (oldVal && newVal === '') {
      submitSearch()
      inputRef.value?.blur()
    }
  },
)
</script>

<template>
  <div class="tools-container">
    <form class="search-form" @submit.prevent="submitSearch">
      <input
        ref="inputRef"
        v-model="searchQuery"
        type="search"
        :disabled="isLoading"
        :placeholder="isLoading ? 'Loading...' : 'Search...'"
      />
      <button
        class="clear-button"
        v-if="searchQuery !== ''"
        type="button"
        :disabled="isLoading"
        @click="searchQuery = ''"
      >
        <span>close_small</span>
      </button>
      <button type="submit" class="small-button" :disabled="isLoading">
        <span>arrow_forward</span>
      </button>
    </form>

    <button type="button" class="small-button" :disabled="isLoading" @click="createForm">
      <span>add</span>
    </button>
  </div>
</template>

<style lang="scss" scoped>
.tools-container {
  display: flex;
  flex-wrap: nowrap;

  .small-button {
    @include lift();

    position: relative;
    background-color: $surface-secondary;

    color: $shadow-color;

    span {
      @include iconize;

      position: absolute;

      top: 8px;
      left: 8px;

      font-size: 24px;
    }
  }

  .search-form {
    display: flex;
    position: relative;
    justify-content: space-between;

    transition:
      transform 0.2s,
      box-shadow 0.2s ease;

    margin-right: 10px;
    margin-bottom: 10px;
    box-shadow: 5px 5px 0px $shadow-color;

    border-radius: 12px;

    background-color: $surface-secondary;
    width: 100%;

    input {
      border: 3px solid $surface-secondary;
      border-right: none;
      border-radius: 12px 0px 0px 12px;
      background-color: $search-color;

      padding: 8px 12px;
      width: 100%;
      font-weight: 500;

      font-size: 15px;
    }

    button {
      transform: none !important;

      margin-left: -10px;
      box-shadow: none !important;
    }

    .clear-button {
      @include iconize;

      position: absolute;
      top: 8px;

      right: 48px;

      opacity: 70%;
      border-radius: 50%;
      background-color: $surface-secondary;
      padding: 0px;

      color: $shadow-color;

      font-size: 24px;
    }

    &:focus-within {
      transform: translateY(-3px);
      box-shadow: 8px 8px 0px $shadow-color;
    }
  }
}
</style>
