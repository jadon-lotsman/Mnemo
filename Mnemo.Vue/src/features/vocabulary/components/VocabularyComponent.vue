<script setup lang="ts">
import { onMounted } from 'vue'
import VocabularyEntryComponent from './VocabularyEntryComponent.vue'
import { useVocabularyStore } from '../stores/VocabularyStore'
import VocabularyToolsComponent from './VocabularyToolsComponent.vue'

const vocabulary = useVocabularyStore()

onMounted(() => {
  vocabulary.fetchVocabulary()
})
</script>

<template>
  <VocabularyToolsComponent :is-loading="vocabulary.isLoading" />
  <div v-if="!vocabulary.isLoading">
    <VocabularyEntryComponent
      v-for="entry in vocabulary.entries"
      :key="entry.foreign"
      :data="entry"
    />
  </div>
</template>

<style lang="scss" scoped></style>
