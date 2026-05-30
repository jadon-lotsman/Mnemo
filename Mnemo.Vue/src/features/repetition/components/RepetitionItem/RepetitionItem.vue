<script lang="ts" setup>
import { ref } from 'vue'
import type { RepetitionTask } from '../../types/RepetitionTask'
import RepetitionRadioItem from './RepetitionRadioItem.vue'

const props = defineProps<{
  listNumber: number
  task: RepetitionTask
}>()

const emits = defineEmits<{
  (e: 'submitAnswer', id: number, answer: string): void
}>()

const userAnswer = ref<string>('')
const placeholder = ref<string>('Type the translation...')

function submitAnswer() {
  emits('submitAnswer', props.task.id, userAnswer.value)
  placeholder.value = userAnswer.value
  userAnswer.value = ''
}
</script>

<template>
  <article class="task">
    <form @submit.prevent="submitAnswer">
      <header>
        <div class="prompt">
          <span>{{ listNumber + 1 }}. Translate </span>
          <span class="bold">"{{ task.prompt }}"</span>
        </div>
        <!-- <div class="part-of-speech">(сущ.)</div> -->
      </header>
      <footer>
        <input
          v-if="task.options.length === 0"
          class="text-input"
          type="text"
          :placeholder="placeholder"
          v-model="userAnswer"
        />
        <div v-else class="option-input">
          <RepetitionRadioItem
            v-for="option in task.options"
            :key="option"
            :value="option"
            v-model="userAnswer"
          />
        </div>
      </footer>
      <button type="submit" class="big-button" :disabled="userAnswer === ''">Submit</button>
    </form>
  </article>
</template>

<style lang="scss" scoped>
.task {
  background-color: $plane-white;
  box-shadow: 5px 5px 0px $shadow;
  border-radius: 12px;

  max-width: 470px;
  min-width: 370px;
  border-radius: 12px;
  margin-bottom: 20px;

  header {
    display: flex;
    justify-content: space-between;
    padding: 12px 15px;

    .prompt {
      color: $gray-font;

      font-size: 16px;

      .bold {
        color: $black-font;
      }
    }
  }

  footer {
    padding: 0px 15px;
    margin-bottom: 18px;

    .text-input {
      background-color: $plane-gray;
      color: $black-font;

      border-radius: 8px;
      padding: 7px 10px;

      width: 100%;

      font-size: 15px;
    }

    .option-input {
      display: flex;
      flex-direction: column;
      gap: 10px;
    }
  }

  .big-button {
    background-color: $plane-gray;
  }
}
</style>
