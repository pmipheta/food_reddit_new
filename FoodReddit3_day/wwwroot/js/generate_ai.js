function generateAiRecipe(apiUrl) {
    const ingredientInput = document.getElementById('ingredient-input').value;
    const btn = document.getElementById('AI-Button');
    const textarea = document.getElementById('body-textarea');

    if (!ingredientInput.trim()) {
        console("fill ingredient before use AI thinking recipes");
        return;
    }

    btn.innerText = "⏳ AI is cooking...";
    btn.disabled = true;

   
    fetch('/Post/GenerateRecipeFromGemini', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `ingredients=${encodeURIComponent(ingredientInput)}`
    })
        .then(response => {
            if (!response.ok) throw new Error("404 Not Found - หา API ไม่เจอ");
            return response.json();
        })
        .then(data => {
            if (data.success) {
                textarea.value = data.recipeText;
            } else {
                console("Error: " + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            
        })
        .finally(() => {
            btn.innerText = "✨ AI Chef: Generate Recipe";
            btn.disabled = false;
        });
}