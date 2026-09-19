<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import ItemSelector from '@/shared/components/ItemSelector/ItemSelector.vue'
import type { SelectorItem } from '@/shared/components/ItemSelector/SelectorItem'
import { useVocabularyStore } from '../stores/VocabularyStore'
import type { VocabularyHeader } from '../types/VocabularyHeader'

defineProps<{
  isLoading?: boolean
}>()

const emit = defineEmits<{
  (e: 'submitSearch', query: string): void
  (e: 'clickCreate'): void
}>()

const vocabularyStore = useVocabularyStore()

const selectedItem = defineModel<SelectorItem<VocabularyHeader> | null>('selected', {
  default: null,
})
const searchQuery = defineModel<string>('searchQuery', { default: '' })

const selectorItems = computed<SelectorItem<VocabularyHeader>[]>(() =>
  vocabularyStore.headers.map((h) => ({
    icon: 'book',
    label: h.name,
    value: h,
  })),
)

const inputRef = ref<HTMLInputElement | null>(null)

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

onMounted(async () => {
  await vocabularyStore.fetchHeadersPage(1, 100)
})
</script>

<template>
  <div class="manager-container">
    <ItemSelector
      icon="book"
      v-model="selectedItem"
      :items="selectorItems"
      @itemChanged="searchQuery = ''"
    />

    <form class="search-form" @submit.prevent="submitSearch">
      <input
        ref="inputRef"
        v-model="searchQuery"
        type="search"
        :disabled="isLoading"
        :placeholder="isLoading ? 'Loading...' : 'Search entries...'"
      />

      <button
        class="clear-button"
        v-if="searchQuery !== ''"
        type="button"
        :disabled="isLoading"
        @click="searchQuery = ''"
      >
        close_small
      </button>

      <button type="submit" class="search-button" :disabled="isLoading">search</button>
    </form>

    <footer class="info-block">
      <span
        >{{ selectedItem?.value.entriesCount ?? 0 }} entries,
        {{ selectedItem?.value.translationsCount ?? 0 }} translations</span
      >
    </footer>
  </div>
</template>

<style lang="scss" scoped>
.manager-container {
  margin-bottom: 15px;
  box-shadow: 5px 5px 0px $shadow-color;

  border-radius: 12px;
  background-color: $surface-primary;

  padding: 8px 6px;

  .search-form {
    display: flex;
    position: relative;

    transition:
      transform 0.2s,
      box-shadow 0.2s ease;

    margin-right: 10px;
    margin-bottom: 8px;

    border-radius: 12px;

    background-color: $surface-secondary;

    width: 100%;

    input {
      border-radius: 8px;
      background-color: $search-color;

      padding: 8px 8px;

      width: 100%;
      max-height: 34px;

      font-size: 15px;
    }

    .search-button {
      @include iconize;

      position: absolute;

      right: 0px;

      border-radius: 8px;

      background-color: $surface-secondary;

      width: 34px;
      height: 34px;

      font-size: 20px;
    }

    .clear-button {
      @include iconize;

      position: absolute;
      top: 6px;
      right: 44px;

      opacity: 70%;
      border-radius: 50%;

      background-color: $surface-secondary;
      color: $icon-color;

      font-weight: 400;
      font-size: 22px;
    }
  }

  .info-block {
    display: flex;
    justify-content: space-between;

    padding-right: 6px;
    padding-left: 6px;

    color: $text-muted;

    font-weight: 250;
    font-size: 15px;
  }
}
</style>
