<script lang="ts" setup>
import { ref } from 'vue'
import type { RepetitionTask } from '../../types/RepetitionTask'
import OptionInput from './OptionInput.vue'
import SentenceInput from './SentenceInput.vue'
import TextInput from './TextInput.vue'

const props = defineProps<{
  listNumber: number
  task: RepetitionTask
  disabled?: boolean
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
    <div>
      <form class="body" @submit.prevent="submitAnswer">
        <header>
          <div class="prompt">
            <div>{{ listNumber }}.</div>

            <div v-if="task.taskType === 'yesorno'">
              <span>Does</span>
              <span class="bold">"{{ task.prompt }}"</span>
              <span>mean</span>
              <span class="bold">"{{ task.option }}"?</span>
            </div>
            <div v-else-if="task.taskType === 'sentence'">
              <span>Put the parts of the sentence in order.</span>
            </div>
            <div v-else-if="task.taskType === 'syllable'">
              <span>Put the syllables in order.</span>
            </div>
            <div v-else>
              <span>Translate</span>
              <span class="bold">"{{ task.prompt }}".</span>
            </div>
          </div>

          <span class="icon" v-if="disabled">{{ task.isCorrect ? 'check_circle' : 'cancel' }}</span>
        </header>
        <footer>
          <div v-if="task.taskType === 'text'">
            <TextInput v-model="userAnswer" :placeholder="placeholder" :disabled="disabled" />
          </div>
          <div v-else-if="task.taskType === 'option'" class="option-input">
            <OptionInput
              v-for="option in task.options"
              v-model="userAnswer"
              :placeholder="placeholder"
              :key="option"
              :value="option"
              :disabled="disabled"
            />
          </div>
          <div v-else-if="task.taskType === 'sentence'">
            <SentenceInput v-model="userAnswer" :parts="task.sentenceParts" :disabled="disabled" />
          </div>
          <div v-else-if="task.taskType === 'syllable'">
            <SentenceInput v-model="userAnswer" :parts="task.syllables" :disabled="disabled" />
          </div>
          <div v-else-if="task.taskType === 'yesorno'" class="option-input">
            <OptionInput
              v-model="userAnswer"
              :placeholder="placeholder"
              :value="'yes'"
              :disabled="disabled"
            />
            <OptionInput
              v-model="userAnswer"
              :placeholder="placeholder"
              :value="'no'"
              :disabled="disabled"
            />
          </div>
        </footer>
        <button type="submit" class="big-button" :disabled="userAnswer === '' || disabled">
          Submit
        </button>
      </form>
    </div>
    <span class="correct" v-if="task.quality != null && task.quality != 5">
      Correct: {{ task.correctAnswer }}
    </span>
  </article>
</template>

<style lang="scss" scoped>
.task {
  max-width: 470px;
  min-width: 370px;
  margin-bottom: 20px;

  .body {
    background-color: $plane-white;
    box-shadow: 5px 5px 0px $shadow;
    border-radius: 12px;

    header {
      position: relative;

      display: flex;
      justify-content: space-between;
      flex-direction: row;
      padding: 12px 15px;

      .prompt {
        position: relative;

        display: flex;
        justify-content: start;

        color: $gray-font;

        font-size: 16px;

        span {
          display: inline-block;
          margin-left: 5px;
        }

        .bold {
          color: $black-font;
        }
      }
    }

    footer {
      padding: 0px 15px;
      margin-bottom: 18px;

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

  .correct {
    display: block;

    color: $gray-font;

    margin-top: 10px;
    margin-left: 10px;

    font-size: 16px;
  }

  .icon {
    @include iconize-text;

    display: block;
    position: absolute;

    top: 10px;
    right: 15px;

    color: $shadow;

    font-size: 24px;
  }
}
</style>
