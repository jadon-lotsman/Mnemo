<script setup lang="ts">
import { ref, watch } from 'vue'

defineProps<{
  isLoading: boolean
}>()

const emit = defineEmits<{
  (e: 'submitSearch', query: string): void
  (e: 'clickCreate'): void
}>()

const searchQuery = ref('')
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

    color: $shadow;
    background-color: $plane-gray;

    span {
      @include iconize-text;

      position: absolute;

      top: 8px;
      left: 8px;
    }
  }

  .search-form {
    position: relative;

    display: flex;
    justify-content: space-between;

    background-color: $plane-gray;
    box-shadow: 5px 5px 0px $shadow;
    width: 100%;

    transition:
      transform 0.2s,
      box-shadow 0.2s ease;

    border-radius: 12px;
    margin-bottom: 12px;

    margin-right: 10px;

    input {
      border: 3px solid $plane-gray;
      border-radius: 12px 0px 0px 12px;
      border-right: none;
      background-color: $clear-white;

      padding: 8px 12px;
      width: 100%;

      font-size: 15px;
      font-weight: 500;
    }

    button {
      transform: none !important;
      box-shadow: none !important;

      margin-left: -10px;
    }

    .clear-button {
      @include iconize-text;

      position: absolute;

      right: 48px;
      top: 8px;

      color: $shadow;
      background-color: $plane-gray;

      padding: 0px;
      border-radius: 50%;

      opacity: 70%;
    }

    &:focus-within {
      box-shadow: 8px 8px 0px $shadow;
      transform: translateY(-3px);
    }
  }
}
</style>
