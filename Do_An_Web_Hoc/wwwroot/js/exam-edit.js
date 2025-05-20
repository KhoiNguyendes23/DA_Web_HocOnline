let quizIndex = 0;

window.onload = () => {
    const modelData = JSON.parse(document.getElementById("quiz-data-json")?.textContent || "null");
    if (Array.isArray(modelData)) {
        modelData.forEach((quiz, i) => {
            addQuiz(quiz, i);
        });
    }
};
function renderLectureOptions(selectedId) {
    const lectures = window.availableLectures || [];
    return lectures.map(l =>
        `<option value="${l.value}" ${selectedId == l.value ? "selected" : ""}>${l.text}</option>`
    ).join('');
}
function addQuiz(quizData = null, index = quizIndex) {
    const quizId = `quiz-${index}`;
    const html = `
    <div class="accordion-item quiz-block" data-quiz-index="${index}">
        <h2 class="accordion-header" id="heading-${quizId}">
            <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse"
                data-bs-target="#collapse-${quizId}" aria-expanded="false" aria-controls="collapse-${quizId}">
                Quiz ${index + 1}: ${quizData?.quizName || 'Chưa đặt tên'}
            </button>
        </h2>
        <div id="collapse-${quizId}" class="accordion-collapse collapse" aria-labelledby="heading-${quizId}">
            <div class="accordion-body">
                <div class="mb-3">
                    <label class="form-label">Tên Quiz</label>
                    <input name="Quizzes[${index}].QuizName" class="form-control quiz-name-input" value="${quizData?.quizName || ''}" oninput="updateQuizLabels()" />
                </div>
                <div class="mb-3">
                    <label class="form-label">Mô tả</label>
                    <input name="Quizzes[${index}].Description" class="form-control" value="${quizData?.description || ''}" />
                </div>
                <div class="mb-3">
                    <label class="form-label">Tổng điểm Quiz</label>
                    <input name="Quizzes[${index}].TotalMarks" class="form-control quiz-points" type="number" value="${quizData?.totalMarks || 0}" onchange="recalculateTotalMarks()" />
                </div>
                <div class="question-list"></div>
                <button type="button" class="btn btn-outline-primary mt-2" onclick="addQuestion(this, ${index})">+ Thêm câu hỏi</button>
                <button type="button" class="btn btn-sm btn-danger mt-2 float-end" onclick="removeQuiz(this)">Xóa Quiz</button>
            </div>
        </div>
    </div>`;

    document.getElementById("quizAccordion").insertAdjacentHTML("beforeend", html);

    if (quizData?.questions?.length) {
        const quizBlock = document.querySelector(`[data-quiz-index='${index}']`);
        quizData.questions.forEach((q, j) => addQuestion(quizBlock.querySelector(".btn-outline-primary"), index, q, j));
    }

    quizIndex++;
    updateQuizLabels();
}

function removeQuiz(btn) {
    btn.closest('.quiz-block').remove();
    updateQuizLabels();
    recalculateTotalMarks();
}

function updateQuizLabels() {
    document.querySelectorAll(".quiz-block").forEach((el, i) => {
        const btn = el.querySelector(".accordion-button");
        const name = el.querySelector(".quiz-name-input")?.value || `Quiz ${i + 1}`;
        btn.innerHTML = `Quiz ${i + 1}: ${name}`;
    });
}

function addQuestion(button, quizIdx, questionData = null, questionCount = null) {
    const questionList = button.closest(".accordion-body").querySelector(".question-list");
    if (questionCount === null) questionCount = questionList.querySelectorAll(".question-block").length;

    const qType = questionData?.questionType || "MCQ";
    const isMCQ = qType === "MCQ";
    const imagePath = questionData?.imagePath || "";

    const html = `
        <div class="border p-3 rounded mb-3 question-block">
            <label class="form-label">Câu hỏi ${questionCount + 1}</label>
            <input name="Quizzes[${quizIdx}].Questions[${questionCount}].QuestionText" value="${questionData?.questionText || ''}" class="form-control mb-2" />

            <label class="form-label">Ảnh minh họa (nếu có)</label>
            <input type="file" accept="image/*" name="Quizzes[${quizIdx}].Questions[${questionCount}].QuestionImage" class="form-control mb-2" onchange="previewImage(this)" />
            <img src="${imagePath}" style="${imagePath ? 'display:block' : 'display:none'}; max-width: 200px; margin-bottom: 10px" class="preview-img" />

            <label class="form-label">Loại câu hỏi</label>
            <select name="Quizzes[${quizIdx}].Questions[${questionCount}].QuestionType" class="form-select mb-3" onchange="toggleQuestionType(this)">
                <option value="MCQ" ${qType === "MCQ" ? 'selected' : ''}>Trắc nghiệm</option>
                <option value="Essay" ${qType === "Essay" ? 'selected' : ''}>Tự luận</option>
            </select>

            <div class="mcq-options" style="${isMCQ ? '' : 'display:none'}">
                <label>Đáp án A</label>
                <input name="Quizzes[${quizIdx}].Questions[${questionCount}].OptionA" value="${questionData?.optionA || ''}" class="form-control mb-2" />
                <label>Đáp án B</label>
                <input name="Quizzes[${quizIdx}].Questions[${questionCount}].OptionB" value="${questionData?.optionB || ''}" class="form-control mb-2" />
                <label>Đáp án C</label>
                <input name="Quizzes[${quizIdx}].Questions[${questionCount}].OptionC" value="${questionData?.optionC || ''}" class="form-control mb-2" />
                <label>Đáp án D</label>
                <input name="Quizzes[${quizIdx}].Questions[${questionCount}].OptionD" value="${questionData?.optionD || ''}" class="form-control mb-2" />
                <label>Đáp án đúng</label>
                <select name="Quizzes[${quizIdx}].Questions[${questionCount}].CorrectAnswer" class="form-select mb-2">
                    <option value="">-- Chọn --</option>
                    <option value="A" ${questionData?.correctAnswer === 'A' ? 'selected' : ''}>A</option>
                    <option value="B" ${questionData?.correctAnswer === 'B' ? 'selected' : ''}>B</option>
                    <option value="C" ${questionData?.correctAnswer === 'C' ? 'selected' : ''}>C</option>
                    <option value="D" ${questionData?.correctAnswer === 'D' ? 'selected' : ''}>D</option>
                </select>
            </div>

            <button type="button" class="btn btn-sm btn-danger mt-2" onclick="this.closest('.question-block').remove()">Xóa câu hỏi</button>
        </div>`;

    questionList.insertAdjacentHTML("beforeend", html);
}

function toggleQuestionType(select) {
    const parent = select.closest(".question-block");
    const mcqDiv = parent.querySelector(".mcq-options");
    mcqDiv.style.display = (select.value === "MCQ") ? "block" : "none";
}

function previewImage(input) {
    const img = input.closest(".question-block").querySelector(".preview-img");
    const file = input.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            img.src = e.target.result;
            img.style.display = "block";
        };
        reader.readAsDataURL(file);
    } else {
        img.src = "";
        img.style.display = "none";
    }
}

function recalculateTotalMarks() {
    const total = Array.from(document.querySelectorAll('.quiz-points')).reduce((sum, input) => {
        return sum + parseInt(input.value || 0);
    }, 0);

    const examTotalInput = document.querySelector("[name='TotalMarks']");
    if (examTotalInput) {
        examTotalInput.value = total;
    }
}

document.addEventListener("DOMContentLoaded", () => {
    const form = document.querySelector("form");
    form.addEventListener("submit", function (e) {
        const examTotal = parseInt(document.querySelector("[name='TotalMarks']")?.value || 0);
        const quizCards = document.querySelectorAll(".quiz-block");

        let quizTotal = 0;
        let valid = true;

        quizCards.forEach(card => {
            const pointsInput = card.querySelector(".quiz-points");
            const quizPoints = parseInt(pointsInput?.value || 0);
            quizTotal += quizPoints;

            const questionList = card.querySelectorAll(".question-block");
            if (questionList.length === 0) {
                alert("Mỗi Quiz cần ít nhất một câu hỏi!");
                valid = false;
                return;
            }
        });

        if (!valid) {
            e.preventDefault();
            return;
        }

        if (examTotal > 0 && quizTotal !== examTotal) {
            e.preventDefault();
            alert(`Tổng điểm các Quiz (${quizTotal}) phải bằng tổng điểm bài kiểm tra (${examTotal})`);
        }
    });
});
