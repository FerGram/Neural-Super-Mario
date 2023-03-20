import torch
import pandas as pd
import matplotlib.pyplot as plt

# Load CSV data
file_name = 'FinalDataFrustration'
df = pd.read_csv(file_name + '.csv')
df = df.astype(int)

# Separate inputs and outputs and make them pytorch tensors
inputs = df.iloc[:, :-1]
outputs = df.iloc[:, -1]

inputs = torch.tensor(inputs.values, dtype=torch.float)
outputs = torch.tensor(outputs.values, dtype=torch.float)

# Training size will be 75% of total data
train_size = int(0.75 * inputs.shape[0])
test_size = inputs.shape[0] - train_size

# Separate in training and testing data
train_inputs = inputs[:train_size]
train_outputs = outputs[:train_size]
test_inputs = inputs[train_size:]
test_outputs = outputs[train_size:]

# Lists for measuring and plotting losses and accuracies
train_losses = []
train_accuracies = []

# Build model
model = torch.nn.Sequential(
    torch.nn.Linear(26, 128),
    torch.nn.ReLU(),
    torch.nn.Linear(128, 256),
    torch.nn.ReLU(),
    torch.nn.Linear(256, 64),
    torch.nn.ReLU(),
    torch.nn.Linear(64, 16),
    torch.nn.ReLU(),
    torch.nn.Linear(16, 1)
)

# Hyperparameters
loss_fn = torch.nn.MSELoss()
optimizer = torch.optim.Adam(model.parameters(), lr=0.00001)

train_epochs = 10000

# Training
for i in range(train_epochs):
    # Calculate predictions (round them to closest int) and loss
    predictions = model(train_inputs)
    loss = loss_fn(predictions, train_outputs.view(-1, 1))

    # Calculate accuracy
    accuracy = (predictions.round() == train_outputs).float().mean()
    train_accuracies.append(accuracy)

    # Store loss
    train_losses.append(loss.item())

    # Backpropagate: currentPoint = currentPoint - learningRate * lossDerivative
    optimizer.zero_grad()
    loss.backward()
    optimizer.step()

# Test neural network performance
test_predictions = model(test_inputs)
print(test_predictions)
test_loss = loss_fn(test_predictions, test_outputs.view(-1, 1))
test_accuracy = (test_predictions.round() == test_outputs).float().mean()

# Print final result
print(f'Test loss: {test_loss:.3f}, Test accuracy: {test_accuracy:.3f}')

# Plot losses over training
plt.plot(train_losses)
plt.xlabel('Iteración')
plt.ylabel('Pérdida')
plt.title('Pérdida durante el entrenamiento')
plt.show()

# Plot accuracy over training
plt.plot(train_accuracies)
plt.xlabel('Iteración')
plt.ylabel('Precisión')
plt.title('Precisión durante el entrenamiento')
plt.show()

# Ask to save the model
save = input("Save model? [y/n]")
if save == 'y':
    dummy_input = torch.randn(1, 26)
    torch.onnx.export(model,
                      dummy_input,
                      (file_name + '.onnx'),
                      export_params=True
                      )
    # torch.save(model.state_dict(), file_name + '.onnx')
