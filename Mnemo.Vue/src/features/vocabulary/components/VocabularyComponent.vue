<script setup lang="ts">
import { onMounted } from 'vue'
import { useVocabularyStore } from '../stores/VocabularyStore'
import VocabularyEntryComponent from './VocabularyEntryComponent.vue'
import VocabularyToolsComponent from './VocabularyToolsComponent.vue'
import type { VocabularyEntry } from '../types/VocabularyEntry.ts'
import { ref } from 'vue'

const vocabulary = useVocabularyStore()

const displayEntries = ref<VocabularyEntry[]>([])

onMounted(async () => {
  await vocabulary.fetchVocabulary()

  displayEntries.value = vocabulary.entries
})

async function handleSearch(query: string) {
  const trimmed = query.trim()
  if (!trimmed) {
    displayEntries.value = vocabulary.entries
    return
  }

  const result = await vocabulary.searchVocabulary(trimmed)
  displayEntries.value = result.length > 0 ? result : vocabulary.entries
}
</script>

<template>
  <VocabularyToolsComponent :is-loading="vocabulary.isLoading" @search="handleSearch" />
  <div v-if="!vocabulary.isLoading">
    <VocabularyEntryComponent v-for="entry in displayEntries" :key="entry.foreign" :data="entry" />
  </div>
</template>

<style lang="scss" scoped></style>
