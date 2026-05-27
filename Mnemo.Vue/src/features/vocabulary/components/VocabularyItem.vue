<script setup lang="ts">
import { capitalize } from '@/shared/utils/StringExtension'
import type { VocabularyEntry } from '../types/VocabularyEntry'
import { ref } from 'vue'
import { useNotify } from '@/shared/composables/useNotify'

const notify = useNotify()

const props = withDefaults(
  defineProps<{
    data: VocabularyEntry
    initialEditing?: boolean
  }>(),
  {
    initialEditing: false,
  },
)

const isEditing = ref(props.initialEditing)

function switchEditing() {
  isEditing.value = !isEditing.value
}

function deleteItem(str: string) {
  notify.custom('Remove', str)
}
</script>

<template>
  <article class="entry" :class="{ 'editor-mode': isEditing }" @click="switchEditing">
    <header>
      <input class="foreign" type="text" v-if="isEditing" :placeholder="data.foreign" @click.stop />
      <div class="foreign" v-else>{{ data.foreign }}</div>

      <input
        class="transcription"
        type="text"
        v-if="isEditing"
        :placeholder="data.transcription"
        @click.stop
      />
      <div class="transcription" v-else>{{ data.transcription }}</div>

      <ol class="translations">
        <div class="editable-item" v-for="translation in data.translations" :key="translation">
          <button v-if="isEditing" @click.stop="deleteItem(translation)">close</button>
          <li>
            {{ translation }}
          </li>
        </div>
      </ol>
      <form class="add-form" v-if="isEditing">
        <input type="text" placeholder="Input a new translation..." @click.stop />
        <button type="button" @click.stop>add</button>
      </form>
    </header>
    <footer>
      <ol v-if="data.examples.length > 0">
        <div class="editable-item" v-for="example in data.examples" :key="example">
          <button v-if="isEditing" @click.stop="deleteItem(example)">close</button>
          <li>
            {{ capitalize(example) }}
          </li>
        </div>
      </ol>
      <form class="add-form" v-if="isEditing">
        <input type="text" placeholder="Input a new example..." @click.stop />
        <button type="button" @click.stop>add</button>
      </form>
    </footer>
  </article>
</template>

<style lang="scss" scoped>
.entry {
  cursor: default;

  display: flex;
  flex-direction: column;
  align-items: stretch;

  transition:
    transform 0.2s,
    box-shadow 0.2s ease;

  color: $black-font;
  background-color: $plane-gray;
  box-shadow: 5px 5px 0px $shadow;

  max-width: 470px;
  min-width: 370px;
  border-radius: 12px;
  margin-bottom: 20px;

  font-size: 16px;

  &.editor-mode {
    box-shadow: 8px 8px 0px $shadow;
    transform: translateY(-3px);
  }

  header {
    display: grid;
    grid-template-columns: 30% 30% 40%;
    background-color: $plane-white;

    padding: 10px 15px;

    border-radius: 12px;

    .foreign {
      grid-column: 1;
    }

    .transcription {
      grid-column: 2;

      margin: 0 5px;
    }

    input.foreign,
    input.transcription {
      color: $black-font;
      background-color: transparent;

      height: 20px;
      width: 80%;

      font-size: 16px;
    }

    .translations {
      grid-column: 3;

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

  &.editor-mode header {
    grid-template-columns: 50% 50%;

    .foreign {
      grid-column: 1;
      grid-row: 1;
    }

    .transcription {
      grid-column: 1;
      grid-row: 1;

      margin: 0px;
      margin-top: 28px;
    }

    .translations {
      grid-column: 2;
      grid-row: 1/3;
    }
  }

  footer {
    padding: 15px;

    li {
      font-size: 16px;
      font-style: italic;

      &::before {
        content: '–';
        padding-right: 10px;
      }
    }

    &:empty {
      padding: 10px;
    }
  }

  &.editor-mode footer {
    padding-bottom: 5px;
    padding-top: 5px;

    ol {
      margin-top: 10px;
    }
  }

  .editable-item {
    display: flex;
    justify-content: start;
    align-items: center;

    button {
      @include iconize-text;

      color: $shadow;
      background-color: transparent;

      box-shadow: none;

      padding: 0px;
      margin-left: -5px;
      margin-right: 5px;

      font-size: 21px;
    }
  }

  .add-form {
    display: flex;
    justify-content: space-between;
    align-items: center;

    grid-column: 1/4;

    color: $gray-font;
    background-color: transparent;

    input {
      background-color: inherit;

      width: 100%;

      font-size: 16px;
      font-style: italic;
    }

    button {
      @include iconize-text;

      color: $shadow;
      background-color: inherit;

      box-shadow: none;

      padding: 4px;
    }

    &::before {
      content: '–';
      padding-right: 10px;
    }
  }
}
</style>
