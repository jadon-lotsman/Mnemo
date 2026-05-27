<script setup lang="ts">
import { capitalize } from '@/shared/utils/StringExtension'
import type { VocabularyEntry } from '../types/VocabularyEntry'

defineProps<{ data: VocabularyEntry }>()
</script>

<template>
  <article class="entry">
    <header>
      <div class="foreign">{{ data.foreign }}</div>
      <div class="transcription">{{ data.transcription }}</div>
      <ol class="translations">
        <li v-for="translation in data.translations" :key="translation">{{ translation }}</li>
      </ol>
    </header>
    <footer v-if="data.examples.length > 0">
      <ol>
        <li v-for="example in data.examples" :key="example">{{ capitalize(example) }}</li>
      </ol>
    </footer>
  </article>
</template>

<style lang="scss" scoped>
.entry {
  cursor: pointer;

  display: flex;
  flex-direction: column;
  align-items: stretch;

  color: $black-font;
  background-color: $plane-gray;
  box-shadow: 5px 5px 0px $shadow;

  max-width: 470px;
  min-width: 370px;
  border-radius: 12px;
  margin-bottom: 20px;

  font-size: 16px;

  header {
    display: grid;
    grid-template-columns: 30% 30% 40%;
    background-color: $plane-white;

    padding: 10px 15px;

    border-radius: 12px;

    .foreign {
      content-visibility: auto;
    }

    .transcription {
      margin: 0 5px;
    }

    .translations {
      display: flex;
      flex-direction: column;

      margin-top: -2px;

      li {
        display: flex;
        flex-direction: row;
        justify-content: space-between;
      }
    }
  }

  footer {
    padding: 15px;

    li {
      font-size: 16px;
      font-style: italic;
    }

    li::before {
      content: '–';
      padding-right: 10px;
    }
  }
}
</style>
