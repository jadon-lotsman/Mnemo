<script setup lang="ts">
import { onMounted } from 'vue'
import { useVocabularyStore } from '../stores/VocabularyStore.ts'
import VocabularyItem from './VocabularyItem.vue'
import VocabularyToolbar from './VocabularyToolbar.vue'
import type {
  VocabularyCreateRequest,
  VocabularyEntry,
  VocabularyPatchRequest,
} from '../types/VocabularyEntry.ts'
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

async function handleNew() {
  const entry: VocabularyEntry = {
    id: -Date.now(),
    foreign: '',
    transcription: '',
    translations: [],
    examples: [],
  }
  displayEntries.value.unshift(entry)
}

async function handleCreate(localId: number, bodyRequest: VocabularyCreateRequest) {
  const result = await vocabulary.addEntry(bodyRequest)

  const index = displayEntries.value.findIndex((e) => e.id === localId)
  if (index !== -1) {
    displayEntries.value.splice(index, 1, result)
  }
}

async function handlePatch(remoteId: number, bodyRequest: VocabularyPatchRequest) {
  const result = await vocabulary.patchEntry(remoteId, bodyRequest)

  const index = vocabulary.entries.findIndex((e) => e.id === remoteId)
  if (index !== -1) {
    displayEntries.value.splice(index, 1, result)
  }
}
</script>

<template>
  <VocabularyToolbar
    :is-loading="vocabulary.isLoading"
    @add-new="handleNew"
    @search="handleSearch"
  />
  <div v-if="!vocabulary.isLoading">
    <VocabularyItem
      v-for="entry in displayEntries"
      :key="entry.foreign"
      :entry="entry"
      @create="handleCreate"
      @patch="handlePatch"
    />
  </div>
</template>

<style lang="scss" scoped></style>
