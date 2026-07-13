<script setup lang="ts">
import { ref, watch } from 'vue'
import type { VocabularySector } from '../types/VocabularySection'

const props = defineProps<{
  isLoading: boolean
  tablets: VocabularySector[]
  disableTablets: boolean
}>()

const emit = defineEmits<{
  (e: 'submitPage', startWord: string, endWord: string): void
}>()

const pageStartWord = ref<string>('')

function submitPage(startWord: string, endWord: string) {
  pageStartWord.value = startWord
  emit('submitPage', startWord, endWord)
}

watch(
  () => props.tablets,
  (newVal) => {
    pageStartWord.value = newVal[0]?.startWord || ''
  },
)
</script>

<template>
  <div v-show="tablets.length > 0" class="navbar">
    <button class="tablet-button">
      <span>sort</span>
    </button>

    <div class="tablet-container">
      <label
        class="tablet-radio"
        :class="{ disabled: isLoading || disableTablets }"
        v-for="tablet in tablets"
        :key="tablet.label"
      >
        <input
          type="radio"
          name="mode"
          :disabled="isLoading || disableTablets"
          :checked="pageStartWord === tablet.startWord"
          @click="submitPage(tablet.startWord, tablet.endWord)"
        />
        <span>{{ tablet.label }}</span>
      </label>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.navbar {
  display: flex;
  user-select: none;

  gap: 8px;

  margin-bottom: 15px;

  .tablet-button,
  .tablet-radio {
    color: $shadow;
    background-color: $plane-white;

    padding: 3px 9px 3px 9px;

    min-width: 35px;
    height: 26px;

    text-align: center;
  }

  .tablet-button {
    position: relative;

    background-color: $plane-gray;

    width: 45px;

    flex-shrink: 0;
    flex-grow: 0;

    span {
      @include iconize-text;

      position: absolute;

      top: 2px;
      left: 9px;
    }
  }

  .tablet-container {
    display: flex;
    justify-content: start;

    gap: 8px;

    .tablet-radio {
      cursor: pointer;

      box-shadow: 5px 5px $shadow;

      border-radius: 12px;

      input {
        display: none;
      }

      input:checked + span {
        color: $gray-font;
      }
    }

    .disabled {
      background-color: $plane-gray;

      span {
        color: $shadow !important;
      }
    }
  }
}
</style>
