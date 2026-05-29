<script lang="ts" setup>
import { ref } from 'vue'
import type { RepetitionTask } from '../types/RepetitionTask'

const props = defineProps<{
  list_number: number
  task: RepetitionTask
}>()

const emits = defineEmits<{
  (e: 'submitAnswer', id: number, answer: string): void
}>()

const userAnswer = ref<string>('')
const placeholder = ref<string>('Translation is...')

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
          <span>{{ list_number }}. Translate </span>
          <span class="bold">"{{ task.prompt }}"</span>
        </div>
        <!-- <div class="part-of-speech">(сущ.)</div> -->
      </header>
      <footer>
        <input type="text" :placeholder="placeholder" v-model="userAnswer" />
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
    padding: 0px 14px 0px 14px;
    margin-bottom: 18px;

    input {
      border: $plane-gray 3px solid;
      border-radius: 10px;
      padding: 5px 10px;

      width: 100%;

      font-size: 16px;
    }
  }

  .big-button {
    background-color: $plane-gray;
  }
}
</style>
