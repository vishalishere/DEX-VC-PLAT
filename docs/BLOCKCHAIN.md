# DecVCPlat Blockchain Integration

This document describes the blockchain integration features of the DecVCPlat platform.

## Overview

DecVCPlat uses blockchain technology to provide transparent, secure, and decentralized funding and voting mechanisms for projects. The platform leverages Ethereum-compatible blockchains and smart contracts to enable:

1. Transparent project funding
2. Decentralized governance through token-based voting
3. Secure fund management and release
4. Immutable record of transactions and votes

## Smart Contract

The platform uses a custom ERC-20 token contract (`DecVCPlatToken`) that extends the standard token functionality with project funding and voting capabilities.

### Key Features

- **Token**: Standard ERC-20 token with additional governance capabilities
- **Project Funding**: Users can fund projects using platform tokens
- **Voting**: Token holders can vote on project proposals with voting power proportional to token holdings
- **Fund Release**: Funds are released to project owners only after successful voting

### Contract Structure

The smart contract includes:

- **Project Management**: Create projects, track funding, and manage fund release
- **Voting System**: Create voting sessions, cast votes, and finalize voting results
- **Role-Based Access Control**: Admin, minter, and pauser roles for platform governance

## Integration Architecture

The blockchain integration is implemented through several components:

1. **IBlockchainService**: Core interface for blockchain interactions
2. **EthereumBlockchainService**: Implementation for Ethereum-compatible blockchains
3. **ITokenContractService**: Interface for token contract interactions
4. **TokenContractService**: Implementation for the DecVCPlat token contract

## Configuration

Blockchain settings are configured in the application's configuration files:

```json
{
  "Blockchain": {
    "ProviderUrl": "https://mainnet.infura.io/v3/your_infura_key",
    "NetworkId": 1,
    "ContractAddress": "0x0000000000000000000000000000000000000000",
    "RequiredConfirmations": 12,
    "GasPriceStrategy": "average"
  }
}
```

## Security Considerations

- Private keys should never be stored in code or configuration files
- Use secure key management solutions for production deployments
- Consider using multi-signature wallets for admin operations
- Implement rate limiting for blockchain operations
- Always verify transactions before considering them final

## Local Development

For local development and testing:

1. Use a local blockchain like Ganache or a testnet like Goerli
2. Configure the `ProviderUrl` to point to your local or test network
3. Deploy the smart contract using tools like Truffle or Hardhat
4. Update the `ContractAddress` in your configuration

## Deployment

To deploy the smart contract to a production network:

1. Compile the contract using Solidity compiler
2. Deploy using the `DeployContractAsync` method of `IBlockchainService`
3. Update the contract address in your application configuration
4. Verify the contract on the blockchain explorer (e.g., Etherscan)

## API Usage Examples

### Creating a Project

```csharp
var transactionHash = await _tokenContractService.CreateProjectAsync(
    adminPrivateKey,
    projectId,
    ownerAddress,
    fundingGoal,
    durationDays);
```

### Funding a Project

```csharp
var transactionHash = await _tokenContractService.FundProjectAsync(
    userPrivateKey,
    projectId,
    amount);
```

### Creating a Voting Session

```csharp
var transactionHash = await _tokenContractService.CreateVotingSessionAsync(
    adminPrivateKey,
    votingSessionId,
    projectId,
    durationDays);
```

### Casting a Vote

```csharp
var transactionHash = await _tokenContractService.VoteAsync(
    userPrivateKey,
    votingSessionId,
    inFavor);
```

## Dependencies

- **Nethereum**: .NET library for Ethereum blockchain interaction
- **OpenZeppelin Contracts**: Secure smart contract components used in our token contract
