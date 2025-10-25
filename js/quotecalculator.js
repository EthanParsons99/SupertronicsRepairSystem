// A counter to ensure that each part added to the form has a unique index for model binding.
let partIndex = 0;

// This function runs once the entire HTML document is loaded and ready.
$(document).ready(function () {

    // Initial calculations and UI setup when the page loads.
    updateTotals();
    toggleNoPartsMessage();

    // Event listener for the "Add Part" button. When clicked, it calls the addPart function.
    $('#addPartBtn').click(function () {
        addPart();
    });

    // Event listener for the labor input fields. Recalculates totals whenever the user types.
    $('#LaborHours, #LaborRate').on('input', function () {
        updateTotals();
    });

    // Event listener for the "Remove Part" buttons.
    // This uses event delegation because the remove buttons are created dynamically.
    $(document).on('click', '.remove-part-btn', function () {
        // Finds the closest parent '.part-item' and removes it from the page.
        $(this).closest('.part-item').remove();
        // Recalculates totals and checks if the "no parts" message should be shown.
        updateTotals();
        toggleNoPartsMessage();
    });

    // Event listener for the quantity and price fields within each part item.
    // Also uses event delegation for dynamically created elements.
    $(document).on('input', '.part-quantity, .part-price', function () {
        // Finds the parent '.part-item' and updates its individual total.
        updatePartTotal($(this).closest('.part-item'));
        // Recalculates the grand totals in the summary box.
        updateTotals();
    });
});

/**
 * Clones the hidden part template, assigns unique names to its form fields,
 * and appends it to the container div.
 */
function addPart() {
    // Get the HTML content from the hidden template div.
    const template = $('#partTemplate').html();
    const newPart = $(template); // Convert the HTML string into a jQuery object.

    // Update the 'name' attributes of the input fields.
    // This is crucial for ASP.NET Core Model Binding to correctly create a List of parts on the server.
    newPart.find('.part-name').attr('name', `Parts[${partIndex}].PartName`);
    newPart.find('.part-number').attr('name', `Parts[${partIndex}].PartNumber`);
    newPart.find('.part-description').attr('name', `Parts[${partIndex}].Description`);
    newPart.find('.part-quantity').attr('name', `Parts[${partIndex}].Quantity`);
    newPart.find('.part-price').attr('name', `Parts[${partIndex}].UnitPrice`);

    // Add the new part item to the visible container.
    $('#partsContainer').append(newPart);

    // Increment the index for the next part that gets added.
    partIndex++;

    // Check if the "no parts" message should be hidden.
    toggleNoPartsMessage();
}

/**
 * Calculates the total for a single part item (quantity * price).
 * @param {jQuery} partItem - The jQuery object representing the '.part-item' div.
 */
function updatePartTotal(partItem) {
    const quantity = parseFloat(partItem.find('.part-quantity').val()) || 0;
    const price = parseFloat(partItem.find('.part-price').val()) || 0;
    const total = quantity * price;

    // Update the read-only total field for that specific part.
    partItem.find('.part-total').val(total.toFixed(2));
}

/**
 * Calculates all totals for the quote summary box by reading values
 * from the labor fields and all part items.
 */
function updateTotals() {
    let partsTotal = 0;
    // Loop through each '.part-item' currently on the page.
    $('.part-item').each(function () {
        // Read the pre-calculated total from each part's total field.
        const total = parseFloat($(this).find('.part-total').val()) || 0;
        partsTotal += total;
    });

    // Read labor values.
    const laborHours = parseFloat($('#LaborHours').val()) || 0;
    const laborRate = parseFloat($('#LaborRate').val()) || 0;
    const laborTotal = laborHours * laborRate;

    // Perform the final calculations.
    const subtotal = partsTotal + laborTotal;
    const taxRate = 0.15; // 15% VAT
    const taxAmount = subtotal * taxRate;
    const grandTotal = subtotal + taxAmount;

    // Update the text of the elements in the summary box, formatted as currency.
    $('#partsTotal').text('R' + partsTotal.toFixed(2));
    $('#laborTotal').text('R' + laborTotal.toFixed(2));
    $('#subtotal').text('R' + subtotal.toFixed(2));
    $('#taxAmount').text('R' + taxAmount.toFixed(2));
    $('#grandTotal').text('R' + grandTotal.toFixed(2));
}

/**
 * Shows or hides the "Click 'Add Part'..." message based on whether any
 * part items exist in the container.
 */
function toggleNoPartsMessage() {
    // Check if there are any elements with the class '.part-item' inside the container.
    const hasParts = $('#partsContainer .part-item').length > 0;
    // If there are parts, hide the message. If not, show it.
    $('#noPartsMessage').toggle(!hasParts);
}